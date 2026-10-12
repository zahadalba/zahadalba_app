import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  const body = await readBody(event);
  try {
    const data = await $fetch<{
      id: string;
      firstName: string;
      lastName: string;
      age: number;
      email: string;
      phone: string;
      accessToken: string;
      refreshToken: string;
    }>("http://localhost:7203/api/auth/sign-in", {
      method: "POST",
      body,
    });

    setCookie(event, "accessToken", data.accessToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 15 * 60,
    });
    setCookie(event, "refreshToken", data.refreshToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 30 * 24 * 60 * 60,
    });

    return {
      id: data.id,
      email: data.email,
    };
  } catch (error) {
    throwNitroError(error, event);
  }
});
