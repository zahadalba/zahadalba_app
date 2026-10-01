<script setup lang="ts">
import type { SignInRequestDto, SignInResponseDto } from '~/types/auth.type';

definePageMeta({
    layout: "auth"
})

const authStore = useAuthStore();

const email = ref("");
const password = ref("");

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

    }
}
</script>

<template>
    <input type="email" v-model="email" name="email" placeholder="Email">
    <input type="password" v-model="password" name="password" placeholder="Пароль">
    <button @click="onSubmit({ email, password })">Войти</button>
    <p>{{ }} {{ }}</p>
</template>