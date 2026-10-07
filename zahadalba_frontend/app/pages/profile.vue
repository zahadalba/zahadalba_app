<script setup lang="ts">
import { useAuth } from '~/services/auth.service';
import type { ApiError } from '~/types/apiError.type';
import type { GetMeResponseDto } from '~/types/auth.type';
import { FetchError } from "ofetch";

definePageMeta({
    middleware: 'auth'
});

const authStore = useAuthStore();

const apiError = ref<ApiError>({
    statusCode: undefined,
    statusMessage: undefined,
    message: undefined,
});

const { getMe, signOut } = useAuth();
const { data: user, error } = useQuery<GetMeResponseDto, FetchError>({
    key: ["auth", "me"],
    query: getMe,
});

apiError.value.statusCode = error.value?.statusCode;
apiError.value.statusMessage = error.value?.data.statusMessage;
apiError.value.message = error.value?.data.message;

const { mutate, isLoading } = useMutation({
    mutation: signOut,

    onSettled: async () => {
        authStore.user = null;
        authStore.isAuthenticated = false;

        await navigateTo("/");
    },
});

const onSubmit = () => {
    mutate();
}
</script>

<template>
    <h3>Профиль</h3>
    <p>Привет, {{ user?.firstName }}!</p>
    <p>ID: {{ user?.id }}</p>
    <button @click="onSubmit" :disabled="isLoading">Выйти</button>
    <p>{{ error?.statusCode }} {{ error?.statusMessage }}</p>
</template>