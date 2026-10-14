import { FetchError } from "ofetch";
import {
  fetchUserProfile,
  handleTokenRefresh,
  throwNitroError,
} from "~~/server/utils/auth";

export default defineEventHandler(async (event) => {
  let accessToken = getCookie(event, "accessToken");
  let refreshToken = getCookie(event, "refreshToken");

  try {
    if (!accessToken && refreshToken) {
      const result = await handleTokenRefresh(event);
      accessToken = result?.accessToken;
      refreshToken = result?.refreshToken;
    }

    if (!accessToken) {
      throw createError({
        statusCode: 401,
        statusMessage: "Не авторизован",
      });
    }
  } catch (error) {
    throwNitroError(error, event);
  }

  try {
    if (accessToken) {
      return await fetchUserProfile(event, accessToken);
    }
  } catch (error) {
    if (
      error instanceof FetchError &&
      error.statusCode === 401 &&
      refreshToken
    ) {
      try {
        const result = await handleTokenRefresh(event);
        accessToken = result?.accessToken;
        refreshToken = result?.refreshToken;

        return await fetchUserProfile(event, accessToken);
      } catch (error) {
        throwNitroError(error, event);
      }
    }
    throwNitroError(error, event);
  }
});
