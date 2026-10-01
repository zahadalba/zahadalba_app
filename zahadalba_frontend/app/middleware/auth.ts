import type { GetMeResponseDto } from "~/types/auth.type";

export default defineNuxtRouteMiddleware(async (to, from) => {
  const headers = useRequestHeaders(["cookie"]);
  const authStore = useAuthStore();

  try {
    const data = await $fetch<GetMeResponseDto>("/api/auth/me", {
      method: "GET",
      headers,
    });

    authStore.user = data;
    authStore.isAuthenticated = true;
  } catch {
    authStore.user = null;
    authStore.isAuthenticated = false;
  }

  if (!authStore.isAuthenticated) {
    return navigateTo("/auth/sign-in");
  }
});
