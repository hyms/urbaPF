<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

class CreateAllTables extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('roles', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->smallInteger("secuencia"); //secuencia de uso
            $table->timestamps();
        });

        Schema::create('usuarios', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("name");
            $table->string("email")->unique();
            $table->string("email_verified_at");
            $table->string("password");
            $table->string("alias",20)->nullable();
            $table->text("detalle")->default("");
            $table->rememberToken();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->smallInteger("secuencia"); //secuencia de uso
            $table->timestamps();
            $table->foreignId('rol')->constrained('roles');
        });

        Schema::create('configuracion', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",200);
            $table->smallInteger("tipo");
            $table->text("valor");
            $table->smallInteger("identificador");
            $table->unsignedInteger("id_padre")->nullable();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('agenda', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",200);
            $table->dateTime("fecha_inicio");
            $table->dateTime("fecha_fin");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('Urbanizacion', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->string("direccion",200);
            $table->string("latitud",200);
            $table->string("longitud",200);
            $table->string("ciudad",50);
            $table->string("departamento",50);
            $table->string("presidente",100)->nullable();
            $table->date("fecha_nacimiento");
            $table->text("detalle");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('cargos', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->text("detalle");
            $table->text("cargo_padre")->nullable();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('vecinos', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("apellidos",100);
            $table->string("nombres",100);
            $table->string("ci",15);
            $table->string("telefono_propio",15);
            $table->string("telefono_referencia",15)->nullable();
            $table->date("fecha_nacimiento");
            $table->date("fecha_ingreso");
            $table->string("profesion",200);
            $table->text("Direccion");
            $table->unsignedInteger("cargo")->nullable();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });



        //modulo de asignacion de material
        Schema::create('Avisos', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->text("detalle");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->smallInteger("secuencia"); //secuencia de uso
            $table->timestamps();
            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('visto_por', function (Blueprint $table) {
            $table->id()->autoIncrement();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('aviso')->constrained('Avisos');
            $table->foreignId('visto_por')->constrained('vecino');
            $table->foreignId('usuario_adm')->constrained('usuarios');
        });
        //modulo
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('asignaciones');
        Schema::dropIfExists('equipo');
        Schema::dropIfExists('permisos');
        Schema::dropIfExists('tipos_permiso_vacacion');
        Schema::dropIfExists('personal');
        Schema::dropIfExists('cargos');
        Schema::dropIfExists('unidades');
        Schema::dropIfExists('unidades_negocio');
        Schema::dropIfExists('empresas');
        Schema::dropIfExists('dias_festivos');
        Schema::dropIfExists('desplegables');
        Schema::dropIfExists('configuracion');
        Schema::dropIfExists('usuarios');
        Schema::dropIfExists('roles');

    }
}
