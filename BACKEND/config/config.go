package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

// Config holds all environment variables for the application.
type Config struct {
	MongoURI         string
	MongoDBName      string
	JWTSecret        string
	JWTRefreshSecret string
	Port             string
	FrontendOrigin   string
}

// Load reads environment variables from .env file and returns a Config.
func Load() *Config {
	_ = godotenv.Load()

	cfg := &Config{
		MongoURI:         os.Getenv("MONGODB_URI"),
		MongoDBName:      os.Getenv("MONGODB_DB_NAME"),
		JWTSecret:        os.Getenv("JWT_SECRET"),
		JWTRefreshSecret: os.Getenv("JWT_REFRESH_SECRET"),
		Port:             os.Getenv("PORT"),
		FrontendOrigin:   os.Getenv("FRONTEND_ORIGIN"),
	}

	if cfg.MongoURI == "" || cfg.JWTSecret == "" || cfg.MongoDBName == "" {
		log.Fatal("❌ Missing required environment variables")
	}

	if cfg.FrontendOrigin == "" {
		cfg.FrontendOrigin = "http://localhost:5173"
	}

	return cfg
}
