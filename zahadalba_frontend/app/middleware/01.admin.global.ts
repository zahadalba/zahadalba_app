export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore();

  if (to.path.startsWith("/admin")) {
    if (!authStore.isAuthenticated) {
      return navigateTo("/auth/sign-in");
    }

    if (authStore.user?.role !== "Admin") {
      return navigateTo("/");
    }
  }
});
