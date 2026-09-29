import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  const body = await readBody(event);

  try {
    const data = await $fetch<{
      id: string;
      email: string;
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
      maxAge: 2 * 60,
    });
    setCookie(event, "refreshToken", data.refreshToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 30 * 24 * 60 * 60,
    });

    return { id: data.id, email: data.email };
  } catch (error) {
    if (error instanceof FetchError) {
      throw createError({
        statusCode: error.statusCode,
        message: error.data.detail,
      });
    }
  }
});
