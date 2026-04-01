<?php
require '/var/www/ordpanel/config.php';
require '/var/www/ordpanel/backend/Database.php';

$t = microtime(true);
try {
    $pdo = Database::getInstance();
    $elapsed = round(microtime(true) - $t, 3);
    echo "ok in {$elapsed}s\n";
    echo 'drivers:' . implode(',', PDO::getAvailableDrivers()) . "\n";
} catch (Exception $e) {
    $elapsed = round(microtime(true) - $t, 3);
    echo "err in {$elapsed}s {$e->getMessage()}\n";
}
