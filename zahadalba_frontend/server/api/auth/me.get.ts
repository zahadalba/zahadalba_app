import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  let accessToken = getCookie(event, "accessToken");
  let refreshToken = getCookie(event, "refreshToken");

  const setCookies = () => {
    if (accessToken && refreshToken) {
      setCookie(event, "accessToken", accessToken, {
        httpOnly: true,
        secure: true,
        sameSite: "strict",
        maxAge: 15 * 60,
      });
      setCookie(event, "refreshToken", refreshToken, {
        httpOnly: true,
        secure: true,
        sameSite: "strict",
        maxAge: 30 * 24 * 60 * 60,
      });
    }
  };

  const deleteCookies = () => {
    deleteCookie(event, "accessToken");
    deleteCookie(event, "refreshToken");
  };

  if (!accessToken && refreshToken) {
    try {
      const data = await $fetch<{
        accessToken: string;
        refreshToken: string;
      }>("http://localhost:7203/api/auth/refresh", {
        method: "POST",
        headers: {
          cookie: `refreshToken=${refreshToken}`,
        },
      });

      accessToken = data.accessToken;
      refreshToken = data.refreshToken;

      setCookies();
    } catch (error) {
      deleteCookies();

      if (error instanceof FetchError) {
        throw createError({
          statusCode: error.statusCode,
          statusMessage: error.data.title,
          message: error.data.detail,
          data: error.data,
        });
      }
    }
  }

  if (!accessToken) {
    throw createError({
      statusCode: 401,
      statusMessage: "Не авторизован",
    });
  }

  try {
    const data = await $fetch<{
      id: string;
      firstName: string;
      lastName: string;
      age: number;
      email: string;
      phone: string;
    }>("http://localhost:7203/api/auth/me", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    });

    return data;
  } catch (error) {
    if (
      error instanceof FetchError &&
      error.statusCode === 401 &&
      refreshToken
    ) {
      try {
        const data = await $fetch<{
          accessToken: string;
          refreshToken: string;
        }>("http://localhost:7203/api/auth/refresh", {
          method: "POST",
          headers: {
            cookie: `refreshToken=${refreshToken}`,
          },
        });

        accessToken = data.accessToken;
        refreshToken = data.refreshToken;

        setCookies();
        const retryData = await $fetch<{
          id: string;
          firstName: string;
          lastName: string;
          age: number;
          email: string;
          phone: string;
        }>("http://localhost:7203/api/auth/me", {
          method: "GET",
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        });

        return retryData;
      } catch (error) {
        deleteCookies();

        if (error instanceof FetchError) {
          throw createError({
            statusCode: error.statusCode,
            statusMessage: error.data.title,
            message: error.data.detail,
            data: error.data,
          });
        }
      }
    }
    deleteCookies();

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
