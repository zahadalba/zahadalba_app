export default defineEventHandler(async (event) => {
  const body = await readBody(event);
  const config = useRuntimeConfig(event);
  try {
    await $fetch("/api/auth/sign-up", {
      baseURL: config.apiBaseUrl,
      method: "POST",
      body,
    });
  } catch (error) {
    throwNitroError(error, event);
  }
});
