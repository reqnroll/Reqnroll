@ECHO OFF

pushd %~dp0

sphinx-autobuild --ignore _build/ . _build/html

popd
