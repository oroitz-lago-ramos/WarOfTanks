import { mkdir, mkdtemp, rm, stat, writeFile } from 'node:fs/promises'
import { tmpdir } from 'node:os'
import { join } from 'node:path'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'

const repository =
  process.env.UNITY_RELEASE_REPOSITORY ?? 'oussema-fatnassi/WarOfTanks'
const assetName = 'waroftanks-webgl-build.tar.gz'
const releaseApiUrl =
  process.env.UNITY_RELEASE_API_URL ??
  `https://api.github.com/repos/${repository}/releases/latest`
const outputPath =
  process.env.UNITY_BUILD_OUTPUT_PATH ??
  fileURLToPath(new URL('../public/UnityBuild/', import.meta.url))

if (process.env.VERCEL !== '1' && process.env.FETCH_UNITY_BUILD !== '1') {
  console.log(
    'Skipping Unity release download outside Vercel. Set FETCH_UNITY_BUILD=1 to fetch it locally.',
  )
  process.exit(0)
}

const releaseResponse = await fetch(releaseApiUrl, {
  headers: {
    Accept: 'application/vnd.github+json',
    'User-Agent': 'WarOfTanks-Vercel-Build',
    'X-GitHub-Api-Version': '2022-11-28',
  },
})

if (!releaseResponse.ok) {
  if (process.env.VERCEL_ENV === 'preview') {
    console.warn(
      `Unity release is not available yet (${releaseResponse.status}); continuing with the frontend preview only.`,
    )
    process.exit(0)
  }

  throw new Error(
    `Unable to find the latest Unity release (${releaseResponse.status}). Run the WebGL Build workflow on main first.`,
  )
}

const release = await releaseResponse.json()
const asset = release.assets?.find((candidate) => candidate.name === assetName)

if (!asset?.browser_download_url) {
  throw new Error(`Latest GitHub Release does not contain ${assetName}.`)
}

const archiveResponse = await fetch(asset.browser_download_url)
if (!archiveResponse.ok) {
  throw new Error(
    `Unable to download ${assetName} (${archiveResponse.status}).`,
  )
}

const temporaryDirectory = await mkdtemp(join(tmpdir(), 'waroftanks-unity-'))
const archivePath = join(temporaryDirectory, assetName)

try {
  await writeFile(archivePath, Buffer.from(await archiveResponse.arrayBuffer()))
  await rm(outputPath, { recursive: true, force: true })
  await mkdir(outputPath, { recursive: true })

  const extraction = spawnSync('tar', ['-xzf', archivePath, '-C', outputPath], {
    encoding: 'utf8',
  })

  if (extraction.status !== 0) {
    throw new Error(
      `Unable to extract Unity release: ${extraction.stderr || extraction.stdout}`,
    )
  }

  await stat(join(outputPath, 'index.html'))
  console.log(`Installed Unity ${release.tag_name} into public/UnityBuild.`)
} finally {
  await rm(temporaryDirectory, { recursive: true, force: true })
}
