import { email, z } from "zod";

export const signUpSchema = z
  .object({
    firstName: z
      .string()
      .trim()
      .min(2, {
        error: "Имя должно содержать минимум 2 символа",
      })
      .max(50, {
        error: "Имя должно содержать максимум 50 символов",
      })
      .regex(/^\p{L}+(?:[ '-]\p{L}+)*$/u, {
        error: "Имя содержит недопустимые символы",
      }),

    lastName: z
      .string()
      .trim()
      .min(2, {
        error: "Фамилия должна содержать минимум 2 символа",
      })
      .max(50, {
        error: "Фамилия должна содержать максимум 50 символов",
      })
      .regex(/^\p{L}+(?:[ '-]\p{L}+)*$/u, {
        error: "Фамилия содержит недопустимые символы",
      }),

    age: z
      .number({
        error: "Введите возраст",
      })
      .int({
        error: "Возраст должен быть целым числом",
      })
      .min(14, {
        error: "Возраст должен быть не менее 14 лет",
      })
      .max(120, {
        error: "Введите корректный возраст",
      }),

    email: z.email({
      error: "Введите корректный email",
    }),

    phone: z
      .string()
      .trim()
      .regex(/^\+?[1-9]\d{9,14}$/, {
        error: "Введите корректный номер телефона",
      }),

    password: z
      .string()
      .min(8, {
        error: "Пароль должен содержать минимум 8 символов",
      })
      .max(64, {
        error: "Пароль должен содержать максимум 64 символа",
      })
      .refine((value) => /[A-Z]/.test(value), {
        error: "Пароль должен содержать заглавную букву",
      })
      .refine((value) => /[a-z]/.test(value), {
        error: "Пароль должен содержать строчную букву",
      })
      .refine((value) => /\d/.test(value), {
        error: "Пароль должен содержать цифру",
      })
      .refine((value) => /[^A-Za-z0-9]/.test(value), {
        error: "Пароль должен содержать специальный символ",
      }),

    confirmPassword: z.string().min(1, {
      error: "Подтвердите пароль",
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    error: "Пароли не совпадают",
    path: ["confirmPassword"],
  });

export type SignUpFormInput = z.infer<typeof signUpSchema>;

export const signInSchema = z.object({
  email: z.email({
    error: "Введите корректный email",
  }),
  password: z
    .string()
    .min(1, {
      error: "Введите пароль",
    })
    .max(64, {
      error: "Пароль должен содержать максимум 64 символа",
    }),
});

export type SugnInFormInput = z.infer<typeof signInSchema>;
