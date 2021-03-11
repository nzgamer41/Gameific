Remove-Item *.pdb
Remove-Item LANGame.zip
Compress-Archive * LANGame.zip
Remove-Item ..\..\..\LANGame.zip
Move-Item LANGame.zip ..\..\..\LANGame.zip