FROM mcr.microsoft.com/dotnet/sdk:10.0

ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /src

EXPOSE 8090

ENTRYPOINT ["dotnet", "run", "--project", "Parser_html-TestJob/Parser_html-TestJob.csproj", "--no-launch-profile"]
