export default defineEventHandler(async (event) => {
  const refreshToken = getCookie(event, "refreshToken");

  if (refreshToken) {
    try {
      // Уведомляем ASP.NET бэкенд, чтобы он отозвал токен в БД
      await $fetch("http://localhost:7203/api/auth/sign-out", {
        method: "POST",
        headers: {
          cookie: `refreshToken=${refreshToken}`,
        },
      });
    } catch {
      // Игнорируем ошибки сети при логауте
    }
  }

  // Очищаем куки в браузере
  deleteCookie(event, "accessToken");
  deleteCookie(event, "refreshToken");

  return { success: true };
});
