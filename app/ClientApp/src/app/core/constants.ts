export class Constants {
      static readonly BASE_URL: string = "http://localhost:4200/api/v1";
      static readonly API = {
          AUTHENTICATION: `${Constants.BASE_URL}/authentication`,
          REGISTRATION: `${Constants.BASE_URL}/registration`,
          TOKEN: `${Constants.BASE_URL}/authentication/token`,
          JWT_TOKEN_KEY: "zora_jwt_token"
      }
}
