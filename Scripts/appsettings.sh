sed -i 's/win10-x64-v{version}.zip/ubuntu.16.10-x64-v{version}.tar.gz/g' EmbyStat.Web/appsettings.json
sed -i 's/0.0.0.0/%system.GitVersion.SemVer%/g' EmbyStat.Web/appsettings.json
sed -i 's/RollbarENV/dev/g' EmbyStat.Web/appsettings.json
sed -i 's/XXXXXXXXXX/$(RollbarKey)/g' EmbyStat.Web/appsettings.json