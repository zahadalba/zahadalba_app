export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore();

  try {
    await restoreAuth();
  } catch (error) {
    console.error("Не удалось восстановить сессию:", error);
  }

  if (authStore.status === "error") {
    throw createError({
      statusCode: 503,
      statusMessage: "Не удалось проверить авторизацию",
      fatal: true,
    });
  }

  if (authStore.status !== "authenticated") {
    return navigateTo("/auth/sign-in");
  }
});
