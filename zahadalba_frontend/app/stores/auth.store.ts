import type { GetMeResponseDto } from "~/types/auth.type";
import type { User } from "~/types/user.type";

type AuthStatus = "unknown" | "authenticated" | "unauthenticated" | "error";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const isAuthenticated = ref(false);
  const isInitialized = ref(false);
  const status = ref<AuthStatus>("unknown");

  const setUser = (data: GetMeResponseDto) => {
    user.value = data;
    isAuthenticated.value = true;
    isInitialized.value = true;
    status.value = "authenticated";
  };

  const clearUser = () => {
    user.value = null;
    isAuthenticated.value = false;
    isInitialized.value = true;
    status.value = "unauthenticated";
  };

  const setError = () => {
    isInitialized.value = true;
    status.value = "error";
  };

  return {
    user,
    isAuthenticated,
    isInitialized,
    status,
    setUser,
    clearUser,
    setError,
  };
});
