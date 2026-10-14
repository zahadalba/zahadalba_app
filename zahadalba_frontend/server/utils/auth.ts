import type { H3Event } from "h3";
import { FetchError } from "ofetch";

export const handleTokenRefresh = async (event: H3Event) => {
  const config = useRuntimeConfig(event);
  let accessToken = getCookie(event, "accessToken");
  let refreshToken = getCookie(event, "refreshToken");

  if (!refreshToken) {
    throw createError({
      statusCode: 401,
      statusMessage: "Не авторизован",
    });
  }

  const data = await $fetch<{
    status: string;
    accessToken: string;
    refreshToken: string;
  }>("/api/auth/refresh", {
    baseURL: config.apiBaseUrl,
    method: "POST",
    headers: {
      cookie: `refreshToken=${refreshToken}`,
    },
  });

  accessToken = data.accessToken;
  setCookie(event, "accessToken", accessToken, {
    httpOnly: true,
    secure: true,
    sameSite: "strict",
    maxAge: 10 * 60,
  });

  if (data.status === "Refreshed") {
    refreshToken = data.refreshToken;
    setCookie(event, "refreshToken", refreshToken, {
      httpOnly: true,
      secure: true,
      sameSite: "strict",
      maxAge: 30 * 24 * 60 * 60,
    });
  }

  return {
    accessToken,
    refreshToken,
  };
};

export const throwNitroError = (error: unknown, event: H3Event) => {
  if (error instanceof FetchError) {
    if (error.statusCode === 401) {
      deleteCookie(event, "accessToken");
      deleteCookie(event, "refreshToken");
    }

    throw createError({
      statusCode: error.statusCode ?? 502,
      statusMessage: error.data?.title ?? "Ошибка запроса",
      message: error.data?.detail ?? error.message,
      data: error.data,
    });
  }

  throw error;
};

export const fetchUserProfile = async (event: H3Event, accessToken: string) => {
  const config = useRuntimeConfig(event);
  const data = await $fetch<{
    id: string;
    firstName: string;
    lastName: string;
    age: number;
    email: string;
    phone: string;
    role: string;
  }>("/api/auth/me", {
    baseURL: config.apiBaseUrl,
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });

  return data;
};
