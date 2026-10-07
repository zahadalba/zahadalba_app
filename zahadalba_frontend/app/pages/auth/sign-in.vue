<script setup lang="ts">
import { useRegleSchema } from '@regle/schemas';
import { FetchError } from "ofetch";
import type { ApiError } from '~/types/apiError.type';
import { useAuth } from '~/services/auth.service';

definePageMeta({
    layout: "auth"
})

const authStore = useAuthStore();

const state = reactive({
    email: "",
    password: "",
});

const apiError = ref<ApiError>({
    statusCode: undefined,
    statusMessage: undefined,
    message: undefined,
});

const { r$ } = useRegleSchema(state, signInSchema)

const { signIn } = useAuth();
const { mutate, isLoading } = useMutation({
    mutation: signIn,

    onSuccess: async (data) => {
        authStore.user = data;
        authStore.isAuthenticated = true;

        await navigateTo("/");
    },

    onError: (error: FetchError) => {
        apiError.value.statusCode = error.statusCode;
        apiError.value.statusMessage = error.data.statusMessage;
        apiError.value.message = error.data.message;
    }
});

const onSubmit = async () => {
    const validationResult = await r$.$validate();
    if (!validationResult.valid) {
        return;
    }

    mutate(r$.$value)
}
</script>

<template>
    <h3>Авторизация</h3>
    <NuxtLink to="/">Главная</NuxtLink>
    <NuxtLink to="/auth/sign-up">Регистарция</NuxtLink>
    <form @submit.prevent="onSubmit">
        <div>
            <input type="email" v-model="state.email" placeholder="Email">
            <span v-if="r$.email.$error">{{ r$.email.$errors[0] }}</span>
        </div>

        <div>
            <input type="password" v-model="state.password" placeholder="Пароль">
            <span v-if="r$.password.$error">{{ r$.password.$errors[0] }}</span>
        </div>

        <button type="submit" :disabled="isLoading">Войти</button>
    </form>
    <p>{{ apiError.statusCode }} {{ apiError.message }} {{ apiError.statusMessage }}</p>
</template>