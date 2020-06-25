<template>

<div class="col-sm-12">

    <div class="wrapper-page">

        <div class="m-t-40 card-box">
            <div class="text-center">
                <h2 class="text-uppercase m-t-0 m-b-30">
                    <a href="#" class="text-success">
                        <span><img :src="images.logo" alt="" height="30"></span>
                    </a>
                </h2>
                <!--<h4 class="text-uppercase font-bold m-b-0">Sign In</h4>-->
            </div>
            <div v-if="m_error">
                <b-alert show dismissible variant="danger" v-for="(value, key) in m_error" :key="key">
                    {{ key }}: {{ value }}
                </b-alert>
            </div>
            <div class="account-content">
                <form class="form-horizontal" @submit.prevent="login">

                    <div class="form-group m-b-20">
                        <div class="col-12">
                            <label>Email address</label>
                            <b-form-input v-model="email" placeholder="Usuario"></b-form-input>
                        </div>
                    </div>

                    <div class="form-group m-b-20">
                        <div class="col-12">
                            <a href="#" class="text-muted pull-right font-14">Forgot your password?</a>
                            <label>Password</label>
                            <b-form-input type="password" v-model="password" placeholder="Contraseña"></b-form-input>
                        </div>
                    </div>

                    <div class="form-group m-b-30">
                        <div class="col-12">
                            <div class="checkbox checkbox-primary">
                                <input id="checkbox5" type="checkbox">
                                <label for="checkbox5">
                                    Remember me
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="form-group account-btn text-center m-t-10">
                        <div class="col-12">
                            <b-button type="submit" variant="primary btn-lg btn-block">Ingresar</b-button>
                        </div>
                    </div>

                </form>

                <div class="clearfix"></div>

            </div>
        </div>
        <!-- end card-box-->

    </div>
    <!-- end wrapper -->

</div>



</template>

<script>

    export default {
        data() {
            return {
                email: '',
                password: '',
                m_error: false,
                images: {
                    logo: require('../assets/images/logo.png')
                }
            }
        },

        methods: {
            async login() {
                await this.$store
                    .dispatch('login', {
                        email: this.email,
                        password: this.password
                    })
                    .then(() => {
                        this.$router.push({name: 'About'})
                    })
                    .catch(err => {
                        console.log(err);
                    });

                try {
                    this.m_error = JSON.parse(this.$store.state.message);
                } catch (e) {
                    this.m_error = {"mensaje":[this.$store.state.message]};
                }

            }
        }
    }
</script>
