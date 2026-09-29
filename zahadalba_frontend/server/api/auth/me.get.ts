import { FetchError } from "ofetch";

export default defineEventHandler(async (event) => {
  let accessToken = getCookie(event, "accessToken");
  let refreshToken = getCookie(event, "refreshToken");

  // Флаг для локального dev-окружения (без HTTPS куки с secure: true браузер отклоняет)

  // 1. Если Access Token отсутствует, но есть Refresh Token — сразу пробуем обновить
  if (!accessToken && refreshToken) {
    try {
      const refreshData = await $fetch<{
        accessToken: string;
        refreshToken: string;
      }>("http://localhost:7203/api/auth/refresh", {
        method: "POST",
        headers: {
          cookie: `refreshToken=${refreshToken}`,
        },
      });

      accessToken = refreshData.accessToken;
      refreshToken = refreshData.refreshToken;

      // Проставляем обновлённый Access Token в куки текущего запроса Nitro
      setCookie(event, "accessToken", accessToken, {
        httpOnly: true,
        sameSite: "strict",
        secure: true,
        maxAge: 2 * 60,
      });

      setCookie(event, "refreshToken", refreshToken, {
        httpOnly: true,
        sameSite: "strict",
        secure: true,
        maxAge: 30 * 24 * 60 * 60,
      });
    } catch {
      deleteCookie(event, "accessToken");
      deleteCookie(event, "refreshToken");

      throw createError({
        statusCode: 401,
        statusMessage: "Unauthorized",
      });
    }
  }

  if (!accessToken) {
    throw createError({
      statusCode: 401,
      statusMessage: "Unauthorized",
    });
  }

  // 2. Делаем запрос к ASP.NET Core бэкенду
  try {
    const data = await $fetch<{ id: string; email: string }>(
      "http://localhost:7203/api/auth/me",
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      },
    );

    return { id: data.id, email: data.email };
  } catch (error) {
    // Если бэкенд вернул 401 (например, Access Token истёк прямо во время запроса)
    if (
      error instanceof FetchError &&
      error.statusCode === 401 &&
      refreshToken
    ) {
      try {
        const refreshData = await $fetch<{
          accessToken: string;
          refreshToken: string;
        }>("http://localhost:7203/api/auth/refresh", {
          method: "POST",
          headers: {
            cookie: `refreshToken=${refreshToken}`,
          },
        });

        accessToken = refreshData.accessToken;
        refreshToken = refreshData.refreshToken;

        setCookie(event, "accessToken", accessToken, {
          httpOnly: true,
          sameSite: "strict",
          secure: true,
          maxAge: 2 * 60,
        });

        setCookie(event, "refreshToken", refreshToken, {
          httpOnly: true,
          sameSite: "strict",
          secure: true,
          maxAge: 30 * 24 * 60 * 60,
        });

        // Повторяем запрос к бэкенду с новым токеном
        const retryData = await $fetch<{ id: string; email: string }>(
          "http://localhost:7203/api/auth/me",
          {
            method: "GET",
            headers: {
              Authorization: `Bearer ${accessToken}`,
            },
          },
        );

        return { id: retryData.id, email: retryData.email };
      } catch {
        deleteCookie(event, "accessToken");
        deleteCookie(event, "refreshToken");

        throw createError({
          statusCode: 401,
          statusMessage: "Unauthorized",
        });
      }
    }

    throw createError({
      statusCode: 401,
      statusMessage: "Unauthorized",
    });
  }
});
