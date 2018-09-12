@ECHO OFF
cd ../
mklink /J RockWeb\Themes\Avalanche Avalanche\Plugin\Theme\Avalanche
mklink /J RockWeb\plugins\com_kfs\Avalanche Avalanche\Plugin\Plugins\Avalanche
echo "make sure the links were created"
pause