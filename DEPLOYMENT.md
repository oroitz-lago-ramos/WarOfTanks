# Deployment — Backend on Render, Frontend on Vercel

Architecture: **Vercel** serves the React/Vite frontend (static) → calls the
**Render** Go API over HTTPS → which talks to **MongoDB Atlas**.

```
Browser ──> Vercel (frontend)  ──HTTPS──>  Render (Go backend)  ──>  MongoDB Atlas
```

The two services are wired by two env vars:
- Frontend `VITE_API_URL`  → the Render backend URL
- Backend `ALLOWED_ORIGINS` → the Vercel frontend URL (CORS)

---

## 1. MongoDB Atlas (free M0 cluster)

1. Create an account at https://cloud.mongodb.com → create a free **M0** cluster.
2. **Database Access** → add a user (username + password).
3. **Network Access** → add IP `0.0.0.0/0` (Render uses dynamic IPs).
4. **Connect → Drivers** → copy the SRV string:
   `mongodb+srv://USER:PASS@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority`

If the password contains reserved URL characters (`@`, `:`, `/`, `%`, etc.),
percent-encode it before putting it in the connection string.

---

## 2. Backend on Render

This repo includes [`render.yaml`](./render.yaml) (a Blueprint), so most config is automatic.

1. Merge the deployment work into `dev`, then open and merge a PR from `dev` to
   `main`. The Blueprint intentionally deploys the production `main` branch.
2. https://render.com → **New → Blueprint** → connect this GitHub repo and use
   `render.yaml` from `main`.
3. Render reads `render.yaml` and proposes the `waroftanks-backend` Docker service.
4. Set the values marked `sync: false` in the dashboard:
   | Key | Value |
   |---|---|
   | `MONGODB_URI` | the Atlas SRV string from step 1 |
   | `ALLOWED_ORIGINS` | your Vercel URL (update it after the frontend deploy), e.g. `https://war-of-tanks.vercel.app` |

   (`MONGODB_DB_NAME=waroftanks` and `APP_ENV=production` are already in the
   Blueprint. Render generates both JWT secrets. **Do not set `PORT`** — Render
   injects it.)
5. Deploy. Commits merged into `main` deploy after the branch checks pass.
6. Verify: `curl https://<your-app>.onrender.com/health` → `{"status":"ok"}`.

> Note: Render free instances sleep after ~15 min idle; the first request then takes ~30–50s to wake.

---

## 3. Frontend on Vercel

This repo includes [`FRONTEND/vercel.json`](./FRONTEND/vercel.json) (Vite preset + SPA rewrites).

1. https://vercel.com → **Add New → Project** → import this repo.
2. **Root Directory** → `FRONTEND`.
3. Framework auto-detects **Vite** (build `npm run build`, output `dist`).
4. **Environment Variables** → add:
   | Key | Value |
   |---|---|
   | `VITE_API_URL` | your Render URL, e.g. `https://waroftanks-backend.onrender.com` |
5. Deploy → you get a URL like `https://war-of-tanks.vercel.app`.

---

## 4. Wire them together (important)

1. Copy the Vercel URL → set it as `ALLOWED_ORIGINS` on Render → redeploy backend.
   (Multiple origins allowed, comma-separated — include the Vercel preview domain too if needed.)
2. Confirm `VITE_API_URL` on Vercel points at the Render URL.
3. Test end-to-end: open the Vercel site, register/login — no CORS errors, requests hit Render.

---

## 5. Update the README

After deploy, record the public URLs in `ReadMe.md`:
- API: `https://<your-app>.onrender.com`
- App: `https://<your-app>.vercel.app`

## 6. Publish and deploy the Unity WebGL build

The `WebGL Build` GitHub workflow publishes every successful `main` build as
the `waroftanks-webgl-build.tar.gz` asset of a GitHub Release. Vercel downloads
the latest release into `FRONTEND/public/UnityBuild` before building the site.

To redeploy Vercel only after the Unity release is ready:

1. In Vercel, open the frontend project → **Settings → Git → Deploy Hooks**.
2. Create a hook named `Unity WebGL release` for branch `main`.
3. Copy the generated hook URL.
4. In GitHub, open the repository → **Settings → Secrets and variables →
   Actions → New repository secret**.
5. Name the secret `VERCEL_DEPLOY_HOOK_URL` and paste the hook URL.
6. Run **Actions → WebGL Build → Run workflow** on `main`, or merge a Unity
   change into `main`.
7. Confirm the workflow publishes a GitHub Release and then triggers a Vercel
   deployment.
8. Open `/play` and confirm `/UnityBuild/index.html` returns the Unity page
   instead of the React application.

## Checklist (issue #34)
- [ ] Atlas M0 cluster created, `0.0.0.0/0` allowed
- [ ] Backend deployed on Render via Blueprint, `/health` returns 200
- [ ] All secrets set as Render env vars (none in code)
- [ ] Auto-deploy from `main` after CI checks enabled
- [ ] `ALLOWED_ORIGINS` set to the Vercel URL (CORS)
- [ ] Frontend deployed on Vercel with `VITE_API_URL` → Render
- [ ] End-to-end login works from the Vercel site
- [ ] Public URLs documented in README
- [ ] `VERCEL_DEPLOY_HOOK_URL` configured in GitHub Actions secrets
- [ ] Latest WebGL workflow published a GitHub Release asset
- [ ] `/play` loads `/UnityBuild/index.html` from the Vercel deployment
