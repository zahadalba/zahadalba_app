export default defineEventHandler(async (event) => {
  const refreshToken = getCookie(event, "refreshToken");
  try {
    await $fetch("http://localhost:7203/api/auth/sign-out", {
      method: "POST",
      headers: {
        cookie: `refreshToken=${refreshToken}`,
      },
    });
  } catch {}

  deleteCookie(event, "accessToken");
  deleteCookie(event, "refreshToken");

  return;
});
