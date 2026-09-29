import { FetchError } from "ofetch";

export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore();

  const headers = useRequestHeaders(["cookie"]);
  console.log("middleware");

  try {
    const data = await $fetch<{ id: string; email: string }>("/api/auth/me", {
      method: "GET",
      headers,
    });

    if (data?.email) {
      authStore.user = data;
      authStore.isAuthenticated = true;
    }
  } catch {
    authStore.user = null;
    authStore.isAuthenticated = false;
  }

  if (!authStore.isAuthenticated) return navigateTo("/auth/sign-in");
});
