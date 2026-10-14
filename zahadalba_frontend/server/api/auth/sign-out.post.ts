export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event);
  const refreshToken = getCookie(event, "refreshToken");
  try {
    await $fetch("/api/auth/sign-out", {
      baseURL: config.apiBaseUrl,
      method: "POST",
      headers: {
        cookie: `refreshToken=${refreshToken}`,
      },
    });
  } finally {
    deleteCookie(event, "accessToken");
    deleteCookie(event, "refreshToken");

    return;
  }
});
