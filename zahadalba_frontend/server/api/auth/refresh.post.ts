import { handleTokenRefresh, throwNitroError } from "~~/server/utils/auth";

export default defineEventHandler(async (event) => {
  try {
    await handleTokenRefresh(event);
  } catch (error) {
    throwNitroError(error, event);
  }
});
