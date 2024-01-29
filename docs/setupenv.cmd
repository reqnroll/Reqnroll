@ECHO OFF

pushd %~dp0

python -m venv .
pip install -r requirements.txt

popd
