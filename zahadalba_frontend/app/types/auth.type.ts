export interface SignInRequestDto {
  email: string;
  password: string;
}

export interface SignInResponseDto {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  email: string;
  phone: string;
}

export interface GetMeResponseDto {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  email: string;
  phone: string;
}
