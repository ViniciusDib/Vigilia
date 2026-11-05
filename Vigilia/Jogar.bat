@echo off
REM Abre o build WebGL do VIGILIA no navegador (servidor local na porta 8080).
REM Deixe esta janela aberta enquanto joga; feche-a para desligar o servidor.
cd /d "%~dp0Builds\WebGL"
if not exist index.html (
  echo Build WebGL nao encontrado. No Unity: Vigilia ^> Build WebGL.
  pause
  exit /b 1
)
start "" http://127.0.0.1:8080/
echo VIGILIA rodando em http://127.0.0.1:8080  (feche esta janela para parar)
python -m http.server 8080 --bind 127.0.0.1
