<?php
session_start();
include("../settings/connect_datebase.php");

// Защита от флуда (не отвечаем на частые запросы)
$ip = $_SERVER['REMOTE_ADDR'];
$time = time();

// Проверка на флуд (более 5 запросов в секунду)
$request_check = $mysqli->query("SELECT * FROM `request_log` WHERE `ip`='$ip'");
if($request_check->num_rows > 0) {
    $req = $request_check->fetch_assoc();
    if($req['last_time'] > ($time - 1)) {
        if($req['count'] >= 5) {
            // Не отвечаем на запросы
            exit();
        } else {
            $mysqli->query("UPDATE `request_log` SET `count`=`count`+1 WHERE `ip`='$ip'");
        }
    } else {
        $mysqli->query("UPDATE `request_log` SET `count`=1, `last_time`=$time WHERE `ip`='$ip'");
    }
} else {
    $mysqli->query("INSERT INTO `request_log` VALUES ('$ip', 1, $time)");
}

$login = $_POST['login'];
$password = $_POST['password'];

// Проверка на количество неудачных попыток
$attempts = $mysqli->query("SELECT COUNT(*) as cnt FROM `login_attempts` WHERE `login`='$login' AND `time` > " . ($time - 900));
$attempt_data = $attempts->fetch_assoc();

if($attempt_data['cnt'] >= 5) {
    // Слишком много попыток - просто не отвечаем или возвращаем ошибку
    echo md5(md5(-1));
    exit();
}

$query_user = $mysqli->query("SELECT * FROM `users` WHERE `login`='".$login."' AND `password`= '".$password."';");

$id = -1;
while($user_read = $query_user->fetch_row()) {
    $id = $user_read[0];
}

if($id != -1) {
    $_SESSION['user'] = $id;
    // Очищаем неудачные попытки при успешном входе
    $mysqli->query("DELETE FROM `login_attempts` WHERE `login`='$login'");
} else {
    // Логируем неудачную попытку
    $mysqli->query("INSERT INTO `login_attempts` (`login`, `ip`, `time`) VALUES ('$login', '$ip', $time)");
}

echo md5(md5($id));
?>