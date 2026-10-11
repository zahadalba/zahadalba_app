import type { GetMeResponseDto } from "~/types/auth.type";
import { appendResponseHeader } from "h3";

export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore();

  try {
    let data: GetMeResponseDto;

    if (import.meta.server) {
      const event = useRequestEvent();

      if (!event) {
        throw new Error("SSR event недоступен");
      }

      const response = await $fetch.raw<GetMeResponseDto>("/api/auth/me", {
        headers: useRequestHeaders(["cookie"]),
      });

      // Передаём cookies из внутреннего ответа
      // в ответ исходной SSR-страницы.
      const setCookies = response.headers.getSetCookie();

      for (const cookie of setCookies) {
        appendResponseHeader(event, "set-cookie", cookie);
      }

      data = response._data!;
    } else {
      data = await $fetch<GetMeResponseDto>("/api/auth/me");
    }

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
