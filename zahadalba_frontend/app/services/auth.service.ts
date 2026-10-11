import type {
  GetMeResponseDto,
  SignInRequestDto,
  SignInResponseDto,
  SignUpRequestDto,
} from "~/types/auth.type";

export const useAuth = () => {
  const signIn = async (dto: SignInRequestDto) => {
    return await $fetch<SignInResponseDto>("/api/auth/sign-in", {
      method: "POST",
      body: dto,
    });
  };

  const signUp = async (dto: SignUpRequestDto) => {
    return await $fetch("/api/auth/sign-up", {
      method: "POST",
      body: dto,
    });
  };

  const getMe = async () => {
    console.log("getMeClient");

    const headers = useRequestHeaders(["cookie"]);
    return await $fetch<GetMeResponseDto>("/api/auth/me", {
      method: "GET",
      headers,
    });
  };

  const signOut = async () => {
    const headers = useRequestHeaders(["cookie"]);
    return await $fetch("/api/auth/sign-out", {
      method: "POST",
      headers,
    });
  };

  return {
    signIn,
    signUp,
    getMe,
    signOut,
  };
};
