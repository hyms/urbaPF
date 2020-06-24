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

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('desplegables', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",200);
            $table->smallInteger("tipo");  //el tipo determinara que desplegable se mostrara o filtrara
            $table->text("valor");
            $table->unsignedInteger("id_padre")->nullable();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('dias_festivos', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",200);
            $table->date("fecha");
            //$table->decimal("dias");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('empresas', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->string("nit",20);
            $table->string("direccion",200);
            $table->string("telefono",15);
            $table->string("celular",15);
            $table->string("fax",15)->nullable();
            $table->string("ciudad",50);
            $table->string("departamento",50);
            $table->string("gerente",100)->nullable();
            $table->string("email",100)->nullable();
            $table->string("web",100)->nullable();
            $table->date("fecha_nacimiento");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('unidades_negocio', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->string("direccion",200);
            $table->string("telefono",15);
            $table->string("celular",15);
            $table->string("fax",15)->nullable();
            $table->string("ciudad",50);
            $table->string("departamento",50);
            $table->string("encargado",100);
            $table->string("email",100)->nullable();
            $table->string("web",100)->nullable();
            $table->date("fecha_nacimiento");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
            $table->foreignId('id_empresa')->constrained('empresas');
        });

        Schema::create('unidades', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->text("detalle");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario')->constrained('usuarios');
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
            $table->foreignId('unidad')->constrained('unidades');
        });

        Schema::create('personal', function (Blueprint $table) {
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

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
            $table->foreignId('unidad')->constrained('unidades');
            $table->foreignId('cargo')->constrained('cargos');
            $table->foreignId('agencia')->constrained('unidades_negocio');
        });

        Schema::create('tipos_permiso_vacacion', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->text("detalle");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('permisos', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->boolean("tiempo_parcial");
            $table->boolean("tiempo_completo");
            $table->unsignedDecimal("numero_dias");
            $table->date("fecha_inicio");
            $table->date("fecha_fin");
            $table->text("observaciones");

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('usuario_adm')->constrained('usuarios');
            $table->foreignId('personal')->constrained('personal');
            $table->foreignId('tipo_permiso')->constrained('tipos_permiso_vacacion');
        });

        //modulo de asignacion de material
        Schema::create('equipo', function (Blueprint $table) {
            $table->id()->autoIncrement();
            $table->string("nombre",100);
            $table->text("detalle");
            $table->text("observaciones")->nullable();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->smallInteger("secuencia"); //secuencia de uso
            $table->timestamps();
            $table->foreignId('usuario_adm')->constrained('usuarios');
        });

        Schema::create('asignaciones', function (Blueprint $table) {
            $table->id()->autoIncrement();

            //control de historial
            $table->smallInteger("estado");
            $table->boolean("indicador"); //Activo,inactivo
            $table->integer("secuencia"); //secuencia de uso
            $table->timestamps();

            $table->foreignId('recibido_por')->constrained('personal');
            $table->foreignId('entregado_por')->constrained('personal');
            $table->foreignId('id_material')->constrained('equipo');
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
