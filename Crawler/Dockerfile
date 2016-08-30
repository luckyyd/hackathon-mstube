From ubuntu:14.04
MAINTAINER irmo <irmowan@gmail.com>

# install packages
RUN apt-get update -y
RUN apt-get install -y apt-utils \
    vim \
    git \
    freetds-dev \
    freetds-bin

# install pip
RUN apt-get install -y python3-pip
RUN apt-get install -y wget unzip

# install pymssql
RUN pip3 install pymssql

# install scrapy
RUN apt-get install -y libxml2-dev libxslt1-dev python-dev 
RUN apt-get install -y libz-dev libffi-dev libssl-dev
RUN pip3 install Cython
RUN pip3 install scrapy
RUN pip3 install requests

RUN cd ~ && wget https://github.com/luckyyd/hackathon-mstube/archive/master.zip
RUN unzip master.zip

# config cron
RUN touch /var/log/cron.log
RUN crontab -l > mycron
RUN echo “* * * * * echo \”hello\” >> ~/hello.txt” >> mycron
RUN echo “0 * * * * source ~/hackathon-mstube/Crawler/youtube/autorun.sh” >> mycron
RUN crontab mycron
RUN rm mycron
RUN cron start
RUN service cron start

# define entrypoint
ENTRYPOINT ["/bin/bash"]
