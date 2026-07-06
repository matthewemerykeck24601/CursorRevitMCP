import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  serverExternalPackages: [
    "@google-cloud/documentai",
    "google-auth-library",
    "google-gax",
    "@grpc/grpc-js",
    "gcp-metadata",
  ],
};

export default nextConfig;
