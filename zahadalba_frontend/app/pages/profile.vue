<script setup lang="ts">
import { useAuth } from '~/services/auth.service';

definePageMeta({
    middleware: 'auth'
});

const authStore = useAuthStore();

const { signOut } = useAuth();

const { mutate, isLoading } = useMutation({
    mutation: signOut,

    onSettled: async () => {
        authStore.clearUser();
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