<script setup lang="ts">
import { useRegleSchema } from '@regle/schemas';
import { FetchError } from "ofetch";
import { useAuth } from '~/services/auth.service';
import type { ApiError } from '~/types/apiError.type';

const state = reactive<SignUpFormInput>({
    firstName: "",
    lastName: "",
    age: 0,
    email: "",
    phone: "",
    password: "",
    confirmPassword: "",
});

const apiError = ref<ApiError>({
    statusCode: undefined,
    statusMessage: undefined,
    message: undefined,
});

const { r$ } = useRegleSchema(state, signUpSchema);

const passwordChecks = computed(() => {
    const password = state.password;
    return {
        length: password.length >= 8 && password.length <= 64,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /\d/.test(password),
        special: /[^A-Za-z0-9]/.test(password),
    };
});

const { signUp } = useAuth();
const { mutate, isLoading } = useMutation({
    mutation: signUp,

    onSuccess: async () => {
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
};
</script>

<template>
    <h3>Регистрация</h3>
    <NuxtLink to="/">Главная</NuxtLink>
    <NuxtLink to="/auth/sign-in">Авторизация</NuxtLink>
    <form @submit.prevent="onSubmit">
        <div>
            <input v-model="state.firstName" type="text" placeholder="Имя">
            <span v-if="r$.firstName.$error">{{ r$.firstName.$errors }}</span>
        </div>

        <div>
            <input v-model="state.lastName" type="text" placeholder="Фамилия">
            <span v-if="r$.lastName.$error">{{ r$.lastName.$errors }}</span>
        </div>

        <div>
            <input v-model.number="state.age" type="number" placeholder="Возраст">
            <span v-if="r$.age.$error">{{ r$.age.$errors }}</span>
        </div>

        <div>
            <input v-model="state.email" type="email" placeholder="Email">
            <span v-if="r$.email.$error">{{ r$.email.$errors }}</span>
        </div>

        <div>
            <input v-model="state.phone" type="tel" placeholder="Телефон">
            <span v-if="r$.phone.$error">{{ r$.phone.$errors }}</span>
        </div>

        <div>
            <input v-model="state.password" type="password" placeholder="Пароль">
            <ul>
                <li :class="{ valid: passwordChecks.length }"> 8–64 символа </li>
                <li :class="{ valid: passwordChecks.uppercase }"> Заглавная буква </li>
                <li :class="{ valid: passwordChecks.lowercase }"> Строчная буква </li>
                <li :class="{ valid: passwordChecks.number }"> Цифра </li>
                <li :class="{ valid: passwordChecks.special }"> Специальный символ </li>
            </ul>
        </div>

        <div>
            <input v-model="state.confirmPassword" type="password" placeholder="Подтверждение пароля">
            <span v-if="r$.confirmPassword.$error">{{ r$.confirmPassword.$errors }}</span>
        </div>
        <button type="submit" :disabled="isLoading">Регистрация</button>
    </form>
    <p>{{ apiError.statusCode }} {{ apiError.statusMessage }}</p>
</template>

<style scoped>
.valid {
    color: green;
}
</style>