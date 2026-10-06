import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  const body = await readBody(event);

  try {
    await $fetch("http://localhost:7203/api/auth/sign-up", {
      method: "POST",
      body,
    });
  } catch (error) {
    if (error instanceof FetchError) {
      throw createError({
        statusCode: error.statusCode,
        statusMessage: error.data.title,
        message: error.data.detail,
        data: error.data,
      });
    }
  }
});
