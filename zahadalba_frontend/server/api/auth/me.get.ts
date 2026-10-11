import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  let accessToken = getCookie(event, "accessToken");
  let refreshToken = getCookie(event, "refreshToken");

  console.log("getMeNitro");

  const setCookies = () => {
    console.log("setCookies called");
    console.log("accessToken exists:", Boolean(accessToken));
    console.log("refreshToken exists:", Boolean(refreshToken));
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
        status: string;
        accessToken: string;
        refreshToken: string;
      }>("http://localhost:7203/api/auth/refresh", {
        method: "POST",
        headers: {
          cookie: `refreshToken=${refreshToken}`,
        },
      });
      console.log("Refresh response status:", data.status);
      console.log("New refresh token received:", Boolean(data.refreshToken));

      if (data.status === "Grace") {
        accessToken = data.accessToken;

        setCookie(event, "accessToken", accessToken, {
          httpOnly: true,
          secure: true,
          sameSite: "strict",
          maxAge: 15 * 60,
        });
      }

      if (data.status === "Refreshed") {
        accessToken = data.accessToken;
        refreshToken = data.refreshToken;

        setCookies();
        console.log("Refresh branch finished");
      }
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
    console.log("Returning profile from Nitro");
    return data;
  } catch (error) {
    if (
      error instanceof FetchError &&
      error.statusCode === 401 &&
      refreshToken
    ) {
      try {
        const data = await $fetch<{
          status: string;
          accessToken: string;
          refreshToken: string;
        }>("http://localhost:7203/api/auth/refresh", {
          method: "POST",
          headers: {
            cookie: `refreshToken=${refreshToken}`,
          },
        });
        console.log("Refresh response status:", data.status);
        console.log("New refresh token received:", Boolean(data.refreshToken));

        if (data.status === "Grace") {
          accessToken = data.accessToken;

          setCookie(event, "accessToken", accessToken, {
            httpOnly: true,
            secure: true,
            sameSite: "strict",
            maxAge: 15 * 60,
          });
        }

        if (data.status === "Refreshed") {
          accessToken = data.accessToken;
          refreshToken = data.refreshToken;

          setCookies();
        }
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
