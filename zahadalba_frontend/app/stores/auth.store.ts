import { defineStore } from "pinia";
import type { User } from "~/types/user.type";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const isAuthenticated = ref(false);

  return {
    user,
    isAuthenticated,
  };
});
