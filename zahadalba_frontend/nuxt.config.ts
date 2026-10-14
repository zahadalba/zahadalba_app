// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: false },
  modules: ["@pinia/nuxt", "@pinia/colada-nuxt"],
  runtimeConfig: {
    apiBaseUrl: "http://localhost:7203",
  },
});
