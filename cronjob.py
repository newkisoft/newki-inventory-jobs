from crontab import CronTab

cron = CronTab(user=True)
job = cron.new(command='dotnet newki-inventory-jobs.dll')
job.minute.every(1)
for item in cron:
    print(item)
cron.write()