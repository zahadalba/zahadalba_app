<script setup lang="ts">
import type { SignInRequestDto, SignInResponseDto } from '~/types/auth.type';
import { FetchError } from "ofetch";

definePageMeta({
    layout: "auth"
})

const authStore = useAuthStore();

const email = ref("");
const password = ref("");

const apiError = ref<FetchError>();

const onSubmit = async (dto: SignInRequestDto) => {
    try {
        const data = await $fetch<SignInResponseDto>("/api/auth/sign-in", {
            method: 'POST',
            body: dto,
        });

        authStore.user = data;
        authStore.isAuthenticated = true;

        return navigateTo("/");
    } catch (error) {
        if (error instanceof FetchError) {
            apiError.value = error;
        }
    }
} 
</script>

<template>
    <input type="email" v-model="email" name="email" placeholder="Email">
    <input type="password" v-model="password" name="password" placeholder="Пароль">
    <button @click="onSubmit({ email, password })">Войти</button>
    <p>{{ apiError?.data.message }} {{ apiError?.statusCode }}</p>
</template>