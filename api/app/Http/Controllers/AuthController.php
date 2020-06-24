<?php

namespace App\Http\Controllers;

use App\Usuarios;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Validator;

class AuthController extends Controller
{

    public function login(Request $request)
    {
        try {

            $validator = Validator::make($request->all(), [
                'email' => 'required',
                'password' => 'required',
            ]);
            if ($validator->fails()) {
                throw new \Exception($validator->errors()->toJson());
            }

            $user = Usuarios::VerificarUsuario($request->email,$request->password);
            if (!$user) {
                return response()->json([
                    'status_code' => 404,
                    'message' => 'Datos invalidos'
                ]);
            }
            $tokenResult = $user->createToken('authToken')->plainTextToken;
            return response()->json([
                'status_code' => 200,
                'user' => $user,
                'token' => $tokenResult,
            ]);
        } catch (\Exception $error) {
            return response()->json([
                'status_code' => 500,
                'message' => $error->getMessage(),
                //'error' => $error,
            ]);
        }
    }
}
