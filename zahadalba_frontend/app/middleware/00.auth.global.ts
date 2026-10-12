export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore();

  if (authStore.isInitialized) {
    return;
  }

  try {
    await restoreAuth();
  } catch (error) {
    console.error("Не удалось восстановить сессию:", error);
  }
});
