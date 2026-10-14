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
  const accessToken = getCookie(event, "accessToken");

  try {
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
});
