<script setup lang="ts">
import { useAuth } from '~/services/auth.service';
import type { ApiError } from '~/types/apiError.type';
import type { GetMeResponseDto } from '~/types/auth.type';
import { FetchError } from "ofetch";

definePageMeta({
    middleware: 'auth'
});

const authStore = useAuthStore();

const { data } = useFetch("/api/auth/me");

const { signOut } = useAuth();

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
    <p>Привет, {{ authStore.user?.firstName }}!</p>
    <p>ID: {{ authStore.user?.id }}</p>
    <button @click="onSubmit" :disabled="isLoading">Выйти</button>
</template>