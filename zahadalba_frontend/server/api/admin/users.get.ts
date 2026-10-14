import { FetchError } from "ofetch";

interface AdminUserResponseDto {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  email: string;
  phone: string;
  role: string;
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event);
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
      const data = await $fetch<Record<string, AdminUserResponseDto>>(
        "/api/admin/users",
        {
          baseURL: config.apiBaseUrl,
          method: "GET",
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        },
      );

      return data;
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

        const data = await $fetch<Record<string, AdminUserResponseDto>>(
          "/api/admin/users",
          {
            baseURL: config.apiBaseUrl,
            method: "GET",
            headers: {
              Authorization: `Bearer ${accessToken}`,
            },
          },
        );

        return data;
      } catch (error) {
        throwNitroError(error, event);
      }
    }
    throwNitroError(error, event);
  }
});
