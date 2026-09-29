export default defineNuxtPlugin(async () => {
  const authStore = useAuthStore();
  const headers = useRequestHeaders(["cookie"]);
  console.log("plugin");

  try {
    const data = await $fetch<{ id: string; email: string }>("/api/auth/me", {
      method: "GET",
      headers,
    });

    if (data?.email) {
      authStore.user = data;
      authStore.isAuthenticated = true;
    }
    return;
  } catch {
    authStore.user = null;
    authStore.isAuthenticated = false;
  }
});
