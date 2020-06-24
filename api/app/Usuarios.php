<?php

namespace App;

use Illuminate\Notifications\Notifiable;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use Laravel\Sanctum\HasApiTokens;
use Illuminate\Foundation\Auth\User as Authenticatable;

class Usuarios extends Authenticatable
{
    use Notifiable, HasApiTokens;

    protected $fillable = ['email', 'password'];

    protected $hidden = [
        'password', 'remember_token',
    ];

    public static function VerificarUsuario($email,$password)
    {
        $user = DB::table('usuarios')->where('email', $email)->first();
        if (!$user) {
            return false;
        }
        if (!Hash::check($password, $user->password, [])) {
            return false;
        }
        return $user;
    }

}
